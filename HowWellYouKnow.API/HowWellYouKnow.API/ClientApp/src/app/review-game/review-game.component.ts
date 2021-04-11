import { Component, Inject, Input, OnInit } from '@angular/core';
import { GameScoreDto } from '../dtos/game-state-dto.model';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UserDto } from '../dtos/user-dto.mode';

@Component({
  selector: 'app-review-game',
  templateUrl: './review-game.component.html',
  styleUrls: ['./review-game.component.css']
})
export class ReviewGameComponent implements OnInit {
  @Input() userScores: GameScoreDto[];

  winnerText: string;
  winnerId: string;
  winnerAvatar: string;
  showImage = false;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) {
  }

  ngOnInit(): void {
    this.getWinnerText();

    if (this.winnerId) {
      this.getWinnerImage(this.winnerId);
    }
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
