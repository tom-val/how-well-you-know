import { HttpClient } from '@angular/common/http';
import { EventEmitter, Inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie';
import { UserDto } from 'src/app/dtos/user-dto.mode';
import { SnackbarService } from './snackbar.service';

@Injectable()
export class LoginService {
    public username$: EventEmitter<string>;

    private currentUser: UserDto;
    private userId: string;

      constructor(
        private cookieService: CookieService,
        private http: HttpClient,
        private snackbarService: SnackbarService,
        @Inject('BASE_URL') private baseUrl: string,
        private router: Router) {
          this.username$ = new EventEmitter();
          if (this.cookieService.hasKey('userId')) {
            this.userId = this.cookieService.get('userId');
            this.setUser();
          }
       }

       login(name: string, avatar: string) {
        this.http.post<string>(this.baseUrl + 'api/user', {name, avatar}).subscribe(result => {
          this.cookieService.put('userId', result);

          this.snackbarService.showSuccess('Login successfull');

          this.userId = result;
          this.currentUser = {
            id: result,
            name,
            avatar,
          };

          this.username$.emit(this.currentUser.name);

          this.router.navigate(['']);
        }, error => this.snackbarService.showError(error));
       }

       setUser() {
        this.http.get<UserDto>(this.baseUrl + 'api/user/' + this.userId +'/name').subscribe(result => {
          this.currentUser = result;
          this.username$.emit(this.currentUser.name);
        });
       }

       updateUserId(userId: string): void {
         this.userId = userId;
         this.setUser();
       }

       getUserId(): string {
         return this.currentUser.id;
       }

       logOut(): void {
         this.cookieService.remove('userId');
         this.username$.emit('');
         this.router.navigate(['login']);
      }
}
