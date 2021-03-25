import { Component, Inject, Input, OnInit } from '@angular/core';
import { GameScoreDto, UserAnswerResultDto } from '../dtos/game-state-dto.model';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { QuestionDto } from '../dtos/question-dto.model';
import { QuestionVariantDto } from '../dtos/questionVariant-dto.model';

@Component({
  selector: 'app-review-question',
  templateUrl: './review-question.component.html',
  styleUrls: ['./review-question.component.css']
})
export class ReviewQuestionComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    @Inject('BASE_URL') private baseUrl: string) {
  }

  @Input() gameId: string;
  @Input() answerResults: UserAnswerResultDto[];
  @Input() question: QuestionDto;
  @Input() userScores: GameScoreDto[];
  @Input() lastQuestion: boolean;

  ngOnInit(): void {

  }

  getList(items: QuestionVariantDto[]) {
      return items.map(x => x.name).join(' ');
  }

  nextQuestion() {
    this.http.post<string>(this.baseUrl + 'api/game/' + this.gameId + '/nextQuestion', {}).subscribe(result => {
      console.log(result);
    }, error => this._snackBar.open(JSON.stringify(error.message)));
  }

  endGame() {
    this.http.post<string>(this.baseUrl + 'api/game/' + this.gameId + '/endGame', {}).subscribe(result => {
      console.log(result);
    }, error => this._snackBar.open(JSON.stringify(error.message)));
  }
}
