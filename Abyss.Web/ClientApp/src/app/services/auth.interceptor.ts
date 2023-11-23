import { HttpErrorResponse, HttpHandler, HttpInterceptor, HttpRequest, HttpEvent } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { from, throwError, Observable } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';

import { AuthService } from './auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    private refreshing?: Promise<void>;
    constructor(private authService: AuthService) { }
    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<any> {
        if (!this.requestNeedsAuth(req)) {
            return this.handle(req, next);
        }
        if (this.refreshing) {
            console.log('Delaying request due to token refresh');
            return from(this.refreshing).pipe(mergeMap(() => {
                console.log('Starting delayed request');
                return this.handle(req, next);
            }));
        } else if (this.authService.tokenNeedsRefresh()) {
            let resolve: () => void;
            this.refreshing = new Promise((r) => resolve = r);
            console.log('Token needs refreshing');
            return from(this.authService.getNewToken()).pipe(mergeMap(() => {
                resolve();
                delete this.refreshing;
                return this.handle(req, next);
            }));
        }
        return this.handle(req, next);
    }

    private handle(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(map((resp) => resp), catchError((resp: any) => {
            return throwError(this.handleError(resp));
        }));
    }

    private handleError(e: any): Error {
        let errMsg: string;
        const logged = false;
        if (e instanceof HttpErrorResponse) {
            const err = (e.error && (e.error.error || JSON.stringify(e.error))) || e.message;
            errMsg = err;
        } else {
            errMsg = e.toString();
        }
        if (!logged) {
            console.error(errMsg);
        }
        return new Error(errMsg);
    }

    private requestNeedsAuth(req: HttpRequest<any>): boolean {
        return !req.url.startsWith('/api/auth');
    }
}
