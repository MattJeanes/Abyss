import { Routes, provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';

import { JwtModule } from '@auth0/angular-jwt';

import { PageNotFoundComponent } from './errors/not-found.component';

export function tokenGetter(): string {
  return localStorage.token;
}

const appRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./home/home.component').then(m => m.HomeComponent),
    data: { pageTitle: 'Home' },
  },
  {
    path: 'login/:scheme',
    loadComponent: () => import('./login.component').then(m => m.LoginComponent),
  },
  {
    path: 'usermanager',
    loadComponent: () => import('./usermanager/usermanager.component').then(m => m.UserManagerComponent),
    data: { pageTitle: 'User Manager' },
  },
  {
    path: 'servermanager',
    loadComponent: () => import('./servermanager/servermanager.component').then(m => m.ServerManagerComponent),
    data: { pageTitle: 'Server Manager' },
  },
  {
    path: 'online',
    loadComponent: () => import('./online/online.component').then(m => m.OnlineComponent),
    data: { pageTitle: 'Online' },
  },
  {
    path: 'whosaid',
    loadComponent: () => import('./whosaid/whosaid.component').then(m => m.WhoSaidComponent),
    data: { pageTitle: 'Who Said' },
  },
  {
    path: 'gpt',
    loadComponent: () => import('./gpt/gpt.component').then(m => m.GPTComponent),
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
