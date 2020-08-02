import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { RouterModule, Routes } from '@angular/router';
import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import { FormsModule } from '@angular/forms';

import { AppComponent } from "./app.component";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";

import { MatButtonModule } from "@angular/material/button";
import { MatDialogModule } from "@angular/material/dialog";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatListModule } from "@angular/material/list";
import { MatSelectModule } from "@angular/material/select";
import { MatSliderModule } from "@angular/material/slider";
import { MatTooltipModule } from "@angular/material/tooltip";

import { CovalentDialogsModule } from "@covalent/core/dialogs";

import { MomentModule } from "ngx-moment";

import { JWT_OPTIONS, JwtHelperService, JwtInterceptor } from "@auth0/angular-jwt";

import { HomeComponent } from "./home/home.component";
import { LoginComponent } from "./login.component";
import { UserManagerComponent } from "./usermanager/usermanager.component";
import { AuthGuard } from "./services/auth.guard";
import { ServerManagerComponent } from "./servermanager/servermanager.component";
import { OnlineComponent } from "./online/online.component";
import { WhoSaidComponent } from "./whosaid/whosaid.component";
import { PageNotFoundComponent } from "./errors/not-found.component";
import { AccountDialogComponent } from './shared/account-dialog.component';

import { AuthService } from './services/auth.service';
import { UserService } from './services/user.service';
import { ServerService } from './services/server.service';
import { ErrorService } from './services/error.service';
import { OnlineService } from './services/online.service';
import { WhoSaidService } from './services/whosaid.service';
import { AuthInterceptor } from "./services/auth.interceptor";

export function JwtTokenGetter(): string {
    return localStorage.token;
}

const routes: Routes = [
    { path: "", component: HomeComponent },
    { path: "login/:scheme", component: LoginComponent },
    { path: "usermanager", component: UserManagerComponent, canActivate: [AuthGuard], data: { permissions: "UserManager" } },
    { path: "servermanager", component: ServerManagerComponent, canActivate: [AuthGuard], data: { permissions: "ServerManager" } },
    { path: "online", component: OnlineComponent },
    { path: "whosaid", component: WhoSaidComponent, canActivate: [AuthGuard], data: { permissions: "WhoSaid" } },
    { path: "**", component: PageNotFoundComponent },
];

@NgModule({
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
    imports: [
        RouterModule.forRoot(routes),
        BrowserModule,
        BrowserAnimationsModule,
        FormsModule,
        HttpClientModule,
        MatButtonModule,
        MatSliderModule,
        CovalentDialogsModule,
        MatSelectModule,
        MatDialogModule,
        MatListModule,
        MatTooltipModule,
        MatIconModule,
        MomentModule,
        MatInputModule,
    ],
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
    entryComponents: [
        AccountDialogComponent
    ],
    bootstrap: [AppComponent],
})
export class AppModule { }
