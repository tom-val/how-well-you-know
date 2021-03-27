import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CookieService } from 'ngx-cookie';
import { UserDto } from '../dtos/user-dto.mode';
import { LoginService } from 'src/services/login.service';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.css']
})
export class ToolbarComponent {
  userName = '';
  constructor(
    private http: HttpClient,
    private cookieService: CookieService,
    private loginService: LoginService,
    @Inject('BASE_URL') private baseUrl: string) {

  }

  ngOnInit() {
    if (this.cookieService.hasKey('userId')) {
      this.setUserName();
    }
    this.loginService.loggedInUser$.subscribe(item => {
      if (item) {
        this.setUserName();
      }
      else {
        this.userName = '';
      }
    });
  }

  setUserName() {
    const userId = this.cookieService.get('userId');
    this.http.get<UserDto>(this.baseUrl + 'api/user/' + userId +'/name').subscribe(result => {
      this.userName = result.name;
    });
  }

  showLogout(): boolean {
    return this.cookieService.hasKey('userId');
  }

  logoutButtonClick() {
    this.loginService.logOut();
  }

}
