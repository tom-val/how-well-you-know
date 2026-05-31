import {
    HttpEvent,
    HttpInterceptor,
    HttpHandler,
    HttpRequest,
} from '@angular/common/http';
import { CookieService } from 'ngx-cookie';
import { Observable } from 'rxjs';

export class AddHeaderInterceptor implements HttpInterceptor {
    constructor(private cookieService: CookieService) {

    }
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        let userId = '';
        if (this.cookieService.hasKey('userId')) {
            userId = this.cookieService.get('userId');
        }

        const clonedRequest = req.clone({ headers: req.headers.append('UserId', userId) });

        // Pass the cloned request instead of the original request to the next handle
        return next.handle(clonedRequest);
    }
}