import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatLegacyListModule as MatListModule } from '@angular/material/legacy-list';
import { MatLegacyTooltipModule as MatTooltipModule } from '@angular/material/legacy-tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatLegacyDialogModule as MatDialogModule } from '@angular/material/legacy-dialog';
import { MatLegacyFormFieldModule as MatFormFieldModule } from '@angular/material/legacy-form-field';

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
import { MatLegacyInputModule as MatInputModule } from '@angular/material/legacy-input';

export function JwtTokenGetter(): string {
    return localStorage.token;
}

@NgModule({
    declarations: [
        AppComponent,
        AccountDialogComponent,
        DialogAlertComponent,
        DialogConfirmComponent,
        DialogPromptComponent,
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        FormsModule,
        AppRoutingModule,
        HttpClientModule,
        MatButtonModule,
        MatListModule,
        MatTooltipModule,
        MatIconModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
    ],
    providers: [
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
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
