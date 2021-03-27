import { EventEmitter, Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { CookieService } from "ngx-cookie";

@Injectable()
export class LoginService {
    public loggedInUser$: EventEmitter<string>;
      constructor(private cookieService: CookieService, private router: Router) {
          this.loggedInUser$ = new EventEmitter();
       }
       updateUser (userId:string):void {
          this.loggedInUser$.emit(userId)
       }

       logOut ():void {
         this.cookieService.remove('userId');
         this.loggedInUser$.emit('');
         this.router.navigate(['login']);
      }
}