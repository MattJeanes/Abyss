import { Routes, provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';

import { JwtModule } from '@auth0/angular-jwt';

import { LoginComponent } from './login.component';
import { PageNotFoundComponent } from './errors/not-found.component';
import { GPTComponent } from './gpt/gpt.component';
import { HomeComponent } from './home/home.component';
import { OnlineComponent } from './online/online.component';
import { ServerManagerComponent } from './servermanager/servermanager.component';
import { UserManagerComponent } from './usermanager/usermanager.component';
import { WhoSaidComponent } from './whosaid/whosaid.component';

export function tokenGetter(): string {
  return localStorage.token;
}

const appRoutes: Routes = [
  {
    path: '',
    component: HomeComponent,
    data: { pageTitle: 'Home' },
  },
  {
    path: 'login/:scheme',
    component: LoginComponent,
  },
  {
    path: 'usermanager',
    component: UserManagerComponent,
    data: { pageTitle: 'User Manager' },
  },
  {
    path: 'servermanager',
    component: ServerManagerComponent,
    data: { pageTitle: 'Server Manager' },
  },
  {
    path: 'online',
    component: OnlineComponent,
    data: { pageTitle: 'Online' },
  },
  {
    path: 'whosaid',
    component: WhoSaidComponent,
    data: { pageTitle: 'Who Said' },
  },
  {
    path: 'gpt',
    component: GPTComponent,
    data: { pageTitle: 'GPT' },
  },
  {
    path: '**',
    component: PageNotFoundComponent,
  },
];
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(appRoutes),
    provideHttpClient(withInterceptorsFromDi()),
    provideAnimations(),
    importProvidersFrom(
      JwtModule.forRoot({
          config: {
              tokenGetter: tokenGetter,
              allowedDomains: ["example.com"],
              disallowedRoutes: ["http://example.com/examplebadroute/"],
          },
      }),
    )
  ]
}
