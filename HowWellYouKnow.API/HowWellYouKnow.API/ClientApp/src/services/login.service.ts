import { EventEmitter, Injectable } from "@angular/core";

@Injectable()
export class LoginService {
    public loggedInUser$: EventEmitter<string>;
      constructor() {
          this.loggedInUser$ = new EventEmitter();
       }
       updateUser (userId:string):void {
          this.loggedInUser$.emit(userId)
       }
}