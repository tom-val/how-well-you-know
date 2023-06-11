import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CookieService } from 'ngx-cookie';
import { UserDto } from '../dtos/user-dto.mode';
import { LoginService } from 'src/services/login.service';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.css']
})
export class ToolbarComponent implements OnInit {
  userName = '';
  constructor(
    private cookieService: CookieService,
    private loginService: LoginService) {

  }

  ngOnInit() {
    this.loginService.username$.subscribe(item => {
      this.userName = item;
    });
  }

  showLogout(): boolean {
    return this.cookieService.hasKey('userId');
  }

  logoutButtonClick() {
    this.loginService.logOut();
  }

}
