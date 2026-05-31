import { Component, EventEmitter, Inject, Input, OnInit, Output } from '@angular/core';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { CreateQuestionDialogComponent } from '../question-dialog/create-question-dialog.component';
import { UserDto } from '../dtos/user-dto.mode';
import { QuestionDto } from '../dtos/question-dto.model';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { SnackbarService } from 'src/services/snackbar.service';

@Component({
  selector: 'app-game-setup',
  templateUrl: './game-setup.component.html',
  styleUrls: ['./game-setup.component.css']
})
export class GameSetupComponent implements OnInit {
  @Input() gameId: string;
  @Input() gameName: string;
  @Input() joinedUsers: UserDto[];
  @Input() questions: QuestionDto[];

  constructor(
    @Inject('BASE_URL') private baseUrl: string,
    private http: HttpClient,
    private errorService: SnackbarService,
    private dialog: MatDialog) {
  }

  ngOnInit(): void {
  }

  startGame() {
    this.http.post<string>(this.baseUrl + 'api/game/' + this.gameId + '/start', {}).subscribe(result => {
      console.log(result);
    }, (error: HttpErrorResponse) => this.errorService.showError(error));
  }

  createNewQuestion() {
    const dialogConfig = new MatDialogConfig();

    dialogConfig.disableClose = true;
    dialogConfig.autoFocus = true;

    dialogConfig.data = {
      gameId: this.gameId
    };

    this.dialog.open(CreateQuestionDialogComponent, dialogConfig);
  }


}
