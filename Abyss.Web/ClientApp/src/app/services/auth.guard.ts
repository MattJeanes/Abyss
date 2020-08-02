import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(private authService: AuthService, private router: Router) { }

    public async canActivate(route: ActivatedRouteSnapshot): Promise<boolean> {
        const permissions = route.data && route.data.permissions as string;
        if (permissions && this.authService.hasPermission(permissions)) {
            return true;
        }
        this.router.navigate(['/']);
        return false;
    }
}
