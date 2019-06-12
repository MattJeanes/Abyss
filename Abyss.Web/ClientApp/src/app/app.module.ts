import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { RouterModule, Routes } from "@angular/router";
import { JWT_OPTIONS, JwtHelperService, JwtInterceptor } from "@auth0/angular-jwt";
import { MomentModule } from "ngx-moment";

import { AppComponent } from "./app.component";
import { PageNotFoundComponent } from "./errors/not-found.component";
import { HomeComponent } from "./home/home.component";
import { LoginComponent } from "./login.component";
import { OnlineComponent } from "./online/online.component";
import { ServerManagerComponent } from "./servermanager/servermanager.component";
import { AuthGuard } from "./services/auth.guard";
import { AuthInterceptor } from "./services/auth.interceptor";
import { AuthService } from "./services/auth.service";
import { ErrorService } from "./services/error.service";
import { OnlineService } from "./services/online.service";
import { ServerService } from "./services/server.service";
import { UserService } from "./services/user.service";
import { WhoSaidService } from "./services/whosaid.service";
import { AccountDialogComponent } from "./shared/account-dialog.component";
import { UserManagerComponent } from "./usermanager/usermanager.component";
import { WhoSaidComponent } from "./whosaid/whosaid.component";

import {
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatOptionModule,
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
    { path: "servermanager", component: ServerManagerComponent, canActivate: [AuthGuard], data: { permissions: "ServerManager" } },
    { path: "online", component: OnlineComponent },
    { path: "whosaid", component: WhoSaidComponent, canActivate: [AuthGuard], data: { permissions: "WhoSaid" } },
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
        MatSelectModule,
        MatDialogModule,
        CovalentDialogsModule,
        MatListModule,
        MatTooltipModule,
        MatIconModule,
        MatOptionModule,
        MomentModule,
        MatInputModule,
    ],
    declarations: [
        AppComponent,
        HomeComponent,
        PageNotFoundComponent,
        LoginComponent,
        AccountDialogComponent,
        UserManagerComponent,
        ServerManagerComponent,
        OnlineComponent,
        WhoSaidComponent,
    ],
    entryComponents: [
        AccountDialogComponent,
    ],
    bootstrap: [AppComponent],
    providers: [
        AuthService,
        UserService,
        ServerService,
        ErrorService,
        OnlineService,
        WhoSaidService,
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
    ],
})
export class AppModule { }
