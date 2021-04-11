import { Component, Inject, OnInit } from '@angular/core';
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
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  image: string;

  constructor(
    private loginService: LoginService,
    private formBuilder: FormBuilder) {
  }

  imageUpdated(image: string) {
    this.image = image;
  }

  loginButtonClick() {
    if (!this.loginForm.valid) {
      return;
    }

    const user = {
      ...this.loginForm.value,
      avatar: this.image
    };

    this.loginService.login(user.name, user.avatar);
  }

  ngOnInit() {
    this.loginForm = this.formBuilder.group({
      name: new FormControl('', [Validators.required, Validators.maxLength(20)])
    });
  }

}
