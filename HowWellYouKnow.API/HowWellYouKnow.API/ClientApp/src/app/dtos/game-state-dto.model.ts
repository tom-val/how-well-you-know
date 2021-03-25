import { CurrentGameState } from "./enums/current-game-state.enum";
import { QuestionVariantDto } from "./questionVariant-dto.model";

export interface GameStateDto {
    gameId: string;
    currentGameState: CurrentGameState;
    currentQuestion: string;
    gameScores: GameScoreDto[];
}

export interface GameScoreDto {
    userId: string;
    name: string;
    currentScore: number;
}

export interface UserAnswerResultDto {
    answerVariants: QuestionVariantDto[];
    guessVariants: QuestionVariantDto[];
    correct: boolean;
    name: string;
    guessName: string;
    userId: string;
}