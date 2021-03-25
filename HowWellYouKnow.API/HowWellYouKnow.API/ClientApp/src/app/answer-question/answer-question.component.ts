import { Component, EventEmitter, Inject, Input, OnInit, Output } from '@angular/core';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { CreateQuestionDialogComponent } from '../question-dialog/create-question-dialog.component';
import { UserDto } from '../dtos/user-dto.mode';
import { QuestionDto } from '../dtos/question-dto.model';
import * as signalR from '@microsoft/signalr';
import { GameScoreDto } from '../dtos/game-state-dto.model';

@Component({
  selector: 'app-answer-question',
  templateUrl: './answer-question.component.html',
  styleUrls: ['./answer-question.component.css']
})
export class AnswerQuestionComponent implements OnInit {
  private hubConnection: signalR.HubConnection;

  constructor(
    @Inject('BASE_URL') private baseUrl: string) {
  }

  ngOnInit(): void {

  }

  @Input() gameId: string;
  @Input() joinedUsers: UserDto[];
  @Input() question: QuestionDto;
  @Input() userScores: GameScoreDto[];

  answerQuestion() {

  }

  guessAnswer() {

  }


}
