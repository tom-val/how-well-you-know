import { Component, Inject, Input, OnInit } from '@angular/core';
import { GameScoreDto, UserAnswerResultDto } from '../dtos/game-state-dto.model';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { QuestionDto } from '../dtos/question-dto.model';
import { QuestionVariantDto } from '../dtos/questionVariant-dto.model';

export interface GuessDto {
  guess: string;
  answer: string;
  correct: boolean;
}

@Component({
  selector: 'app-review-question',
  templateUrl: './review-question.component.html',
  styleUrls: ['./review-question.component.css']
})
export class ReviewQuestionComponent implements OnInit {
  @Input() gameId: string;
  @Input() answerResults: UserAnswerResultDto[];
  @Input() question: QuestionDto;
  @Input() userScores: GameScoreDto[];
  @Input() lastQuestion: boolean;

  constructor(
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    @Inject('BASE_URL') private baseUrl: string) {
  }

  ngOnInit(): void {

  }

  getList(items: QuestionVariantDto[]) {
      return items.map(x => x.name).join(' ');
  }

  getAnswerPairs(result: UserAnswerResultDto): GuessDto[] {
    if (result.answerVariants.length === 1 && result.guessVariants.length === 1) {
        return [{
          guess: result.guessVariants[0].name,
          answer: result.answerVariants[0].name,
          correct: result.correct
        }];
    }

  const guessVariants = result.guessVariants.map(v => v.notation);
  const answerVariants = result.answerVariants.map(v => v.notation);

  const guesses = result.guessVariants.map(v => {
    const correct =  answerVariants.includes(v.notation);

    if (correct) {
      const answer = result.answerVariants.find(av => av.notation === v.notation);
      return {
        guess: v.name,
        answer: answer.name,
        correct: true
      };
    }
    return {
      guess: v.name,
      answer: '-',
      correct: false
    };
  });

  const answers = result.answerVariants.filter(v => !guessVariants.includes(v.notation))
  .map(v => ({
      guess: '-',
      answer: v.name,
      correct: false
    }));

    return guesses.concat(answers);
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
