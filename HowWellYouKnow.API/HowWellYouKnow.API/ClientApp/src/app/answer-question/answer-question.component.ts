import { Component, EventEmitter, Inject, Input, OnInit, Output } from '@angular/core';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { CreateQuestionDialogComponent } from '../question-dialog/create-question-dialog.component';
import { UserDto } from '../dtos/user-dto.mode';
import { QuestionDto } from '../dtos/question-dto.model';
import * as signalR from '@microsoft/signalr';
import { GameScoreDto } from '../dtos/game-state-dto.model';
import { MatListOption } from '@angular/material/list';
import { QuestionVariantDto } from '../dtos/questionVariant-dto.model';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CookieService } from 'ngx-cookie';

@Component({
  selector: 'app-answer-question',
  templateUrl: './answer-question.component.html',
  styleUrls: ['./answer-question.component.css']
})
export class AnswerQuestionComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    private cookieService: CookieService,
    @Inject('BASE_URL') private baseUrl: string) {
  }

  @Input() gameId: string;
  @Input() joinedUsers: UserDto[];
  @Input() question: QuestionDto;
  @Input() userScores: GameScoreDto[];

  selectedVariants: string[];
  guessing: boolean;
  guessUsers: UserDto[];
  guessedUsers =  new Array<string>();
  guessingDone: boolean;
  guessingUser: UserDto;

  ngOnInit(): void {
    const currentUserId = this.cookieService.get('userId');
    this.guessUsers = this.joinedUsers.filter(x => x.id !== currentUserId);
    console.log(this.guessUsers);
  }

  answerQuestion() {
    const questionRequest = {
      questionId: this.question.id,
      questionVariants: this.selectedVariants
    }
    this.http.post<string>(this.baseUrl + 'api/game/' + this.gameId + '/answer/answer', questionRequest).subscribe(result => {
      console.log(result);
    }, error => this._snackBar.open(JSON.stringify(error.message)));

    this.guessing = true;
    this.selectedVariants = [];
    this.nextGuessUser();
  }

  nextGuessUser() {
    const availableUsers = this.guessUsers.filter(x => !this.guessedUsers.includes(x.id));
    if (availableUsers.length === 0) {
      console.log("Guessing done");
      this.guessingDone = true;
    }
    else {
      this.guessingUser = availableUsers[0];
    }
  }

  guessAnswer() {
    const guessRequest = {
      questionId: this.question.id,
      guessUser: this.guessingUser.id,
      questionVariants: this.selectedVariants
    }
    this.http.post<string>(this.baseUrl + 'api/game/' + this.gameId + '/answer/guess', guessRequest).subscribe(result => {
      console.log(result);
    }, error => this._snackBar.open(JSON.stringify(error.message)));

    this.guessedUsers.push(this.guessingUser.id);
    this.selectedVariants = [];
    this.nextGuessUser();
  }
}
