import * as jose from "jose";
import { DynamoDBClient } from "@aws-sdk/client-dynamodb";
import {
  DynamoDBDocumentClient,
  PutCommand,
  UpdateCommand,
  DeleteCommand,
} from "@aws-sdk/lib-dynamodb";

const COGNITO_USER_POOL_ID = process.env.COGNITO_USER_POOL_ID;
const CONNECTIONS_TABLE_NAME = process.env.CONNECTIONS_TABLE_NAME;
const REGION = process.env.AWS_REGION || "eu-central-1";

const ddb = DynamoDBDocumentClient.from(new DynamoDBClient({}));

let jwks;

function verifyToken(token) {
  if (!jwks) {
    const jwksUrl = `https://cognito-idp.${REGION}.amazonaws.com/${COGNITO_USER_POOL_ID}/.well-known/jwks.json`;
    jwks = jose.createRemoteJWKSet(new URL(jwksUrl));
  }
  return jose.jwtVerify(token, jwks, {
    issuer: `https://cognito-idp.${REGION}.amazonaws.com/${COGNITO_USER_POOL_ID}`,
  });
}

/**
 * API Gateway WebSocket integration for $connect / $disconnect / subscribe.
 * Tracks each open socket in the connections table so the API can broadcast
 * "game-changed" signals. Auth is done here on $connect: an invalid token
 * yields a non-2xx status, which rejects the handshake.
 */
export const handler = async (event) => {
  const { routeKey, connectionId } = event.requestContext;

  try {
    if (routeKey === "$connect") {
      const token = event.queryStringParameters?.token ?? "";
      if (!token) return { statusCode: 401, body: "Missing token" };

      const { payload } = await verifyToken(token);
      const userId = payload.sub;
      if (!userId) return { statusCode: 401, body: "Invalid token" };

      // 2h TTL is a safety net for sockets that close without a $disconnect event.
      const ttl = Math.floor(Date.now() / 1000) + 2 * 60 * 60;
      await ddb.send(
        new PutCommand({
          TableName: CONNECTIONS_TABLE_NAME,
          Item: { connection_id: connectionId, user_id: userId, ttl },
        })
      );
      return { statusCode: 200, body: "Connected" };
    }

    if (routeKey === "$disconnect") {
      await ddb.send(
        new DeleteCommand({
          TableName: CONNECTIONS_TABLE_NAME,
          Key: { connection_id: connectionId },
        })
      );
      return { statusCode: 200, body: "Disconnected" };
    }

    if (routeKey === "subscribe") {
      const body = event.body ? JSON.parse(event.body) : {};
      const gameId = body.gameId;
      if (!gameId) return { statusCode: 400, body: "Missing gameId" };

      await ddb.send(
        new UpdateCommand({
          TableName: CONNECTIONS_TABLE_NAME,
          Key: { connection_id: connectionId },
          UpdateExpression: "SET game_id = :g",
          ExpressionAttributeValues: { ":g": gameId },
        })
      );
      return { statusCode: 200, body: "Subscribed" };
    }

    return { statusCode: 200, body: "OK" };
  } catch (error) {
    console.error(`[ws-connections] ${routeKey} failed:`, error.message);
    // Rejecting the handshake on $connect; for other routes a 500 just drops the message.
    return { statusCode: routeKey === "$connect" ? 401 : 500, body: "Error" };
  }
};
