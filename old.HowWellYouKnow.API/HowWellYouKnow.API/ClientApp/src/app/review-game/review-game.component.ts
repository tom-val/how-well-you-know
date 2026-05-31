import { Component, Inject, Input, OnInit } from '@angular/core';
import { GameScoreDto } from '../dtos/game-state-dto.model';
import { HttpClient } from '@angular/common/http';
import { UserDto } from '../dtos/user-dto.mode';
import { createEmitter, IEmitter } from 'particle-explosions-3.0.1';
import { interval, Subscription } from 'rxjs';
import { LoginService } from 'src/services/login.service';

@Component({
  selector: 'app-review-game',
  templateUrl: './review-game.component.html',
  styleUrls: ['./review-game.component.css']
})
export class ReviewGameComponent implements OnInit {
  @Input() userScores: GameScoreDto[];

  winnerText: string;
  currentUserId: string;
  winnerId: string;
  winnerAvatar: string;
  showImage = false;
  subscription: Subscription;
  emitter: IEmitter;
  
  constructor(
    private http: HttpClient,
    private loginService: LoginService,
    @Inject('BASE_URL') private baseUrl: string) {
  }

  ngOnInit(): void {
    this.getWinnerText();

    this.currentUserId = this.loginService.getUserId();

    if (this.winnerId) {
      this.getWinnerImage(this.winnerId);
    }
  }

  explode(): void {
    const canvas = document.getElementById('myCanvas') as HTMLCanvasElement;
    const ctx = canvas.getContext('2d')

    this.emitter = createEmitter(ctx)
    this.emitter.explode(250, {
      color: ['#FF5733', '#B43D23', '#FF5733', '#693428']
    })

    const source = interval(1500);
    this.subscription = source.subscribe(val =>  this.emitter.explode(250, {
      color: ['#FF5733', '#B43D23', '#FF5733', '#693428'] 
    }));
  }

  isWinner() {
    return this.winnerId === this.currentUserId;
  }

  getWinnerText(): void {
    if (this.userScores.every(u => u.currentScore === this.userScores[0].currentScore)) {
      this.winnerText = 'Draw';
      return;
    }

    let highestScore = -1;
    let highestUser = null;

    this.userScores.forEach(user => {
      if (user.currentScore > highestScore) {
        highestScore = user.currentScore;
        highestUser = user.name;
        this.winnerId = user.userId;
      }
    });

    this.winnerText =  `Winner is ${highestUser}`;
  }

  getWinnerImage(userId: string): void {
    this.http.get<UserDto>(this.baseUrl + 'api/user/' + userId + '/avatar').subscribe(result => {
      this.winnerAvatar = result.avatar;
      this.showImage = true;
    });
  }
}
