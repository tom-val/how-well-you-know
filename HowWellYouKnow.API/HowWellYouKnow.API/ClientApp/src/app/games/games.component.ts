import { Component, Inject } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

@Component({
  selector: 'app-games',
  templateUrl: './games.component.html',
  styleUrls: ['./games.component.css']
})
export class GamesComponent {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private _snackBar: MatSnackBar,
    private formBuilder: FormBuilder,
    private router: Router) {
  }

  gameForm: FormGroup;

  createNewGame() {
    if (!this.gameForm.valid) {
      return;
    }

      this.http.post<string>(this.baseUrl + 'api/game', this.gameForm.value).subscribe(result => {

        this._snackBar.open('Game created successfully', null, {
          duration: 5000,
        });

        this.router.navigate([`/game/${result}`]);
      }, error => this._snackBar.open(JSON.stringify(error.message)));
  }


  ngOnInit() {
    this.gameForm = this.formBuilder.group({
      name: new FormControl('', [Validators.required, Validators.maxLength(20)])
    });
  }

}
