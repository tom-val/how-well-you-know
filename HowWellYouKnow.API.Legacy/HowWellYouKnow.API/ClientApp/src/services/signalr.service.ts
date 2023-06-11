import {  EventEmitter, Inject, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { GameStateDto } from 'src/app/dtos/game-state-dto.model';
import { QuestionDto } from 'src/app/dtos/question-dto.model';
import { UserDto } from 'src/app/dtos/user-dto.mode';

@Injectable()
export class SignalrService {
  public questionAdded$ = new EventEmitter<QuestionDto>();
  public userAdded$ = new EventEmitter<UserDto>();
  public gameStateChanged$ = new EventEmitter<GameStateDto>();

  private connections = new Array<signalR.HubConnection>();

  constructor(@Inject('BASE_URL') private baseUrl: string) {}

  public startUsersConnection = (gameId: string) => {
    const connection = this.startConnection(this.baseUrl + 'users');
    connection.on(gameId, (data: UserDto) => {
      this.userAdded$.emit(data);
    });

    this.connections.push(connection);
  };

  public startQuestionsConnection = (gameId: string) => {
    const connection = this.startConnection(this.baseUrl + 'questions');
    connection.on(gameId, (data: QuestionDto) => {
      this.questionAdded$.emit(data);
    });

    this.connections.push(connection);
  };

  public startGameStateConnection = (gameId: string) => {
    const connection = this.startConnection(this.baseUrl + 'gameState');
    connection.on(gameId, (data: GameStateDto) => {
      this.gameStateChanged$.emit(data);
    });

    this.connections.push(connection);
  };

  public diconnectAll =  () => {
    this.connections.forEach(connection => connection.stop());
  };

  private startConnection = (url: string): signalR.HubConnection  => {
    const hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .build();
    hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err));
      return hubConnection;
  };
}
