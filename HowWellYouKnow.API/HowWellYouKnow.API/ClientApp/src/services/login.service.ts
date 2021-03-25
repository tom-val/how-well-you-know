import { EventEmitter } from "@angular/core";

export class LoginService {
    public loggedInUser$: EventEmitter<string>;
      constructor() {
          this.loggedInUser$ = new EventEmitter();
       }
       updateUser (userId:string):void {
          this.loggedInUser$.emit(userId)
       }
}