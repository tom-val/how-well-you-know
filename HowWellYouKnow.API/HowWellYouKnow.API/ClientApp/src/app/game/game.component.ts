import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { GameDto } from '../dtos/game-dto.models';
import { QuestionDto } from '../dtos/question-dto.model';
import { GameStateDto } from '../dtos/game-state-dto.model';
import * as signalR from '@microsoft/signalr';
import { UserDto } from '../dtos/user-dto.mode';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private _snackBar: MatSnackBar,
    private route:ActivatedRoute,) {
  }
  private hubConnection: signalR.HubConnection;
  
    gameId: string;
    game: GameDto;
    gameState: GameStateDto;
    lastQuestionId: string;

    public startConnection = () => {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(this.baseUrl + 'gameState')
        .build();
      this.hubConnection
        .start()
        .then(() => console.log('Connection started'))
        .catch(err => console.log('Error while starting connection: ' + err));
    }
  
    public addGameStateListener = () => {
      this.hubConnection.on(this.gameId, (data: GameStateDto) => {
        this.gameState = data;
      });
    }

  ngOnInit() {
    this.gameId = this.route.snapshot.paramMap.get('gameId');
    
    this.startConnection();
    this.addGameStateListener();

    this.http.get<GameDto>(this.baseUrl + 'api/game/' + this.gameId).subscribe(result => {
      this.game = result;   
      this.setLastQuestion();
    }, error => this._snackBar.open(JSON.stringify(error.message)));

    this.http.get<GameStateDto>(this.baseUrl + 'api/game/' + this.gameId + '/state').subscribe(result => {
      this.gameState = result;
    }, error => this._snackBar.open(JSON.stringify(error.message)));
  }

  setLastQuestion() {
    if (this.game.questions.length > 0) {
      this.game.questions = this.game.questions.sort((one, two) => (one.name > two.name ? 1 : -1));
      this.lastQuestionId = this.game.questions[this.game.questions.length - 1].id;
    } 
  }

  updateQuestions(question: QuestionDto ) {
    this.game.questions.push(question);
    this.setLastQuestion();
  }

  updateUsers(user: UserDto ) {
    this.game.joinedUsers.push(user);
  }

  getCurrentQuestion(): QuestionDto {
    if(!this.game) {
      return {} as QuestionDto;
    }
    return this.game.questions.find(x => x.id === this.gameState.currentQuestion);
  }

}
