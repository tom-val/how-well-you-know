import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { GameDto } from '../dtos/game-dto.models';
import { QuestionDto } from '../dtos/question-dto.model';
import { GameStateDto } from '../dtos/game-state-dto.model';
import { UserDto } from '../dtos/user-dto.mode';
import { SignalrService } from 'src/services/signalr.service';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent implements OnInit, OnDestroy {
  gameId: string;
  game: GameDto;
  gameState: GameStateDto;

  constructor(
    private http: HttpClient,
    private signalrService: SignalrService,
    @Inject('BASE_URL') private baseUrl: string,
    private _snackBar: MatSnackBar,
    private route: ActivatedRoute) {
  }

  ngOnDestroy(): void {
    this.signalrService.diconnectAll();
  }

  ngOnInit() {
    this.gameId = this.route.snapshot.paramMap.get('gameId');

    this.signalrService.startGameStateConnection(this.gameId);
    this.signalrService.startQuestionsConnection(this.gameId);
    this.signalrService.startUsersConnection(this.gameId);

    this.signalrService.questionAdded$.subscribe(question => {
      this.game.questions.push(question);
      this.game.questions = this.game.questions.sort(x => x.order);
    });

    this.signalrService.userAdded$.subscribe(user => {
      this.game.joinedUsers.push(user);
    });

    this.signalrService.gameStateChanged$.subscribe(gameState => {
      this.gameState = gameState;
    });


    this.http.get<GameDto>(this.baseUrl + 'api/game/' + this.gameId).subscribe(result => {
      this.game = result;
    }, error => this._snackBar.open(JSON.stringify(error.message)));

    this.http.get<GameStateDto>(this.baseUrl + 'api/game/' + this.gameId + '/state').subscribe(result => {
      this.gameState = result;
    }, error => this._snackBar.open(JSON.stringify(error.message)));
  }

  getCurrentQuestion(): QuestionDto {
    if (!this.game) {
      return {} as QuestionDto;
    }
    return this.game.questions.find(x => x.id === this.gameState.currentQuestion);
  }

}
