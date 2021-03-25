import { Component, Inject } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import * as signalR from '@microsoft/signalr';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CookieService } from 'ngx-cookie';
import { Router } from '@angular/router';
import { LoginService } from 'src/services/login.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private _snackBar: MatSnackBar,
    private cookieService: CookieService,
    private loginService: LoginService,
    private formBuilder: FormBuilder,
    private router: Router) {
  }

  loginForm: FormGroup;

  loginButtonClick() {
    if (!this.loginForm.valid) {
      return;
    }

      this.http.post<string>(this.baseUrl + 'api/user', this.loginForm.value).subscribe(result => {
        this.cookieService.put('userId', result);
        
        this._snackBar.open('Login successfull', null, {
          duration: 5000,
        });

        this.loginService.updateUser(result);

        this.router.navigate(['']);
      }, error => this._snackBar.open(JSON.stringify(error.message)));
  }

  ngOnInit() {
    this.loginForm = this.formBuilder.group({
      name: new FormControl('', [Validators.required, Validators.maxLength(20)])
    });
  }

}
