import { QuestionDto } from "./question-dto.model";
import { UserDto } from "./user-dto.mode";

export interface GameDto {
    gameId: string;
    name: string;
    joinedUsers: UserDto[];
    questions: QuestionDto[];
}