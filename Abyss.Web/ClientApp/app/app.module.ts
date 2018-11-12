import { HttpClientModule } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { RouterModule, Routes } from "@angular/router";
import { JwtModule } from "@auth0/angular-jwt";

import { AppComponent } from "./app.component";
import { AppService } from "./app.service";
import { PageNotFoundComponent } from "./errors/not-found.component";
import { HomeComponent } from "./home/home.component";
import { LoginComponent } from "./login.component";
import { LoginDialogComponent } from "./shared/login-dialog.component";

import {
    MatButtonModule,
    MatDialogModule,
    MatSelectModule,
    MatSliderModule,
} from "@angular/material";

import { CovalentDialogsModule } from "@covalent/core";

export function JwtTokenGetter(): string {
    return localStorage.token;
}

export const ROUTES: Routes = [
    { path: "", component: HomeComponent },
    { path: "login/:scheme", component: LoginComponent },
    { path: "test", loadChildren: "./test/test.module#TestModule" },
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
    ],
    declarations: [
        AppComponent,
        HomeComponent,
        PageNotFoundComponent,
        LoginComponent,
        LoginDialogComponent,
    ],
    entryComponents: [
        LoginDialogComponent,
    ],
    bootstrap: [AppComponent],
    providers: [
        AppService,
    ],
})
export class AppModule { }
