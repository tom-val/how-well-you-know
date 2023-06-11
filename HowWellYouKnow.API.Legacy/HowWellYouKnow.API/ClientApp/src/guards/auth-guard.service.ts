import { Injectable } from '@angular/core';
import { Router, CanActivate } from '@angular/router';
import { CookieService } from 'ngx-cookie';
@Injectable()
export class AuthGuardService implements CanActivate {
  constructor(private cookieService: CookieService, public router: Router) {}
  canActivate(): boolean {
    if (!this.cookieService.hasKey('userId')) {
      this.router.navigate(['login']);
      return false;
    }
    return true;
  }
}