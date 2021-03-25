import { Component, Inject, Input, OnInit } from '@angular/core';
import { GameScoreDto } from '../dtos/game-state-dto.model';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-review-game',
  templateUrl: './review-game.component.html',
  styleUrls: ['./review-game.component.css']
})
export class ReviewGameComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private _snackBar: MatSnackBar,
    @Inject('BASE_URL') private baseUrl: string) {
  }

  @Input() userScores: GameScoreDto[];

  ngOnInit(): void {

  }
}
