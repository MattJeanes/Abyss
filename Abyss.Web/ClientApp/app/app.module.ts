import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { RouterModule, Routes } from "@angular/router";
import { JwtModule } from "@auth0/angular-jwt";

import { AppComponent } from "./app.component";
import { PageNotFoundComponent } from "./errors/not-found.component";
import { HomeComponent } from "./home/home.component";
import { LoginComponent } from "./login.component";
import { AuthGuard } from "./services/auth.guard";
import { AuthInterceptor } from "./services/auth.interceptor";
import { AuthService } from "./services/auth.service";
import { UserService } from "./services/user.service";
import { AccountDialogComponent } from "./shared/account-dialog.component";
import { UserManagerComponent } from "./usermanager/usermanager.component";

import {
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatListModule,
    MatSelectModule,
    MatSliderModule,
    MatTooltipModule,
} from "@angular/material";

import {
    CovalentDialogsModule,
} from "@covalent/core";

export function JwtTokenGetter(): string {
    return localStorage.token;
}

export const ROUTES: Routes = [
    { path: "", component: HomeComponent },
    { path: "login/:scheme", component: LoginComponent },
    { path: "usermanager", component: UserManagerComponent, canActivate: [AuthGuard], data: { permissions: "UserManager" } },
    { path: "**", component: PageNotFoundComponent },
];

@NgModule({
    imports: [
        RouterModule.forRoot(ROUTES),
        BrowserModule,
        BrowserAnimationsModule,
        FormsModule,
        HttpClientModule,
        MatButtonModule,
        MatSliderModule,
        CovalentDialogsModule,
        JwtModule.forRoot({
            config: {
                tokenGetter: JwtTokenGetter,
            },
        }),
        MatSelectModule,
        MatDialogModule,
        CovalentDialogsModule,
        MatListModule,
        MatTooltipModule,
        MatIconModule,
    ],
    declarations: [
        AppComponent,
        HomeComponent,
        PageNotFoundComponent,
        LoginComponent,
        AccountDialogComponent,
        UserManagerComponent,
    ],
    entryComponents: [
        AccountDialogComponent,
    ],
    bootstrap: [AppComponent],
    providers: [
        AuthService,
        UserService,
        AuthGuard,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true,
        },
    ],
})
export class AppModule { }
