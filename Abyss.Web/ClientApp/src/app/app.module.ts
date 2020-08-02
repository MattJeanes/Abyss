import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';

import { TdDialogService } from '@covalent/core/dialogs';

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
} from './services';

export function JwtTokenGetter(): string {
  return localStorage.token;
}

@NgModule({
  declarations: [
    AppComponent,
    AccountDialogComponent,
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
  ],
  providers: [
    AuthService,
    UserService,
    ErrorService,
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
    TdDialogService,
    TitleService,
  ],
  entryComponents: [AccountDialogComponent],
  bootstrap: [AppComponent],
})
export class AppModule { }
