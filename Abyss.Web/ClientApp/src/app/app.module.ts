import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';

import {
    JWT_OPTIONS,
    JwtHelperService,
    JwtInterceptor,
} from '@auth0/angular-jwt';

import { AppRoutingModule } from './app-routing.module';

import { AccountDialogComponent } from './shared';

import {
    AuthService,
    AuthGuard,
    UserService,
    ErrorService,
    AuthInterceptor,
    TitleService,
    DialogService,
    DialogAlertComponent,
    DialogConfirmComponent,
    DialogPromptComponent,
} from './services';
import { MatInputModule } from '@angular/material/input';

export function JwtTokenGetter(): string {
    return localStorage.token;
}

@NgModule({ declarations: [
        AppComponent,
        AccountDialogComponent,
        DialogAlertComponent,
        DialogConfirmComponent,
        DialogPromptComponent,
    ],
    bootstrap: [AppComponent], imports: [BrowserModule,
        BrowserAnimationsModule,
        FormsModule,
        AppRoutingModule,
        MatButtonModule,
        MatListModule,
        MatTooltipModule,
        MatIconModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule], providers: [
        AuthService,
        UserService,
        ErrorService,
        DialogService,
        AuthGuard,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true,
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: JwtInterceptor,
            multi: true,
        },
        {
            provide: JWT_OPTIONS,
            useValue: {
                tokenGetter: JwtTokenGetter,
            },
        },
        JwtHelperService,
        TitleService,
        provideHttpClient(withInterceptorsFromDi()),
    ] })
export class AppModule { }
