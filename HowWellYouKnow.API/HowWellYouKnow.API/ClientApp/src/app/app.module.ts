import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {  MatAutocompleteModule, MatBadgeModule, MatBottomSheetModule, MatButtonModule, MatButtonToggleModule, MatCardModule, MatCheckboxModule, MatChipsModule, MatDatepickerModule, MatDialogModule, MatDividerModule, MatExpansionModule, MatGridListModule, MatIconModule, MatInputModule, MatListModule, MatMenuModule, MatNativeDateModule, MatPaginatorModule, MatProgressBarModule, MatProgressSpinnerModule, MatRadioModule, MatRippleModule, MatSelectModule, MatSidenavModule, MatSliderModule, MatSlideToggleModule, MatSnackBarModule, MatSortModule, MatStepperModule, MatTableModule, MatTabsModule, MatToolbarModule, MatTooltipModule, MatTreeModule } from '@angular/material';
import { A11yModule } from '@angular/cdk/a11y';
import { OverlayModule } from '@angular/cdk/overlay';
import { PortalModule } from '@angular/cdk/portal';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { CdkStepperModule } from '@angular/cdk/stepper';
import { CdkTableModule } from '@angular/cdk/table';
import { CdkTreeModule } from '@angular/cdk/tree';
import { LoginComponent } from './login/login.component';
import { CookieModule, CookieService } from 'ngx-cookie';
import { AddHeaderInterceptor } from 'src/interceptors/add-header-interceptor';
import { AuthGuardService } from 'src/guards/auth-guard.service';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { LoginService } from 'src/services/login.service';
import { GamesComponent } from './games/games.component';
import { GameComponent } from './game/game.component';
import { CreateQuestionDialogComponent } from './question-dialog/create-question-dialog.component';
import { GameSetupComponent } from './game-setup/game-setup.component';
import { AnswerQuestionComponent } from './answer-question/answer-question.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    CounterComponent,
    FetchDataComponent,
    ToolbarComponent,
    GamesComponent,
    GameComponent,
    CreateQuestionDialogComponent,
    GameSetupComponent,
    AnswerQuestionComponent
  ],
  imports: [
    ReactiveFormsModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    MatCardModule,
    A11yModule,
    CdkStepperModule,
    CdkTableModule,
    CdkTreeModule,
    MatAutocompleteModule,
    MatBadgeModule,
    MatBottomSheetModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatStepperModule,
    MatDatepickerModule,
    MatDialogModule,
    MatDividerModule,
    MatExpansionModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatNativeDateModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatRippleModule,
    MatSelectModule,
    MatSidenavModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatSortModule,
    MatTableModule,
    MatTabsModule,
    MatToolbarModule,
    MatTooltipModule,
    MatTreeModule,
    OverlayModule,
    PortalModule,
    ScrollingModule,
    RouterModule.forRoot([
      { path: '', component: GamesComponent, canActivate: [AuthGuardService] },
      { path: 'game/:gameId', component: GameComponent, canActivate: [AuthGuardService] },
      { path: 'login', component: LoginComponent, pathMatch: 'full' }
    ]),
    BrowserAnimationsModule,
    CookieModule.forRoot()
  ],
  providers: [
    AuthGuardService,
    LoginService,
    {
    provide: HTTP_INTERCEPTORS,
    useClass: AddHeaderInterceptor,
    multi: true,
    deps: [CookieService]
  }],
  bootstrap: [AppComponent],
  entryComponents: [CreateQuestionDialogComponent]
})
export class AppModule { }
