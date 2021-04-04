import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatBadgeModule } from '@angular/material/badge';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatNativeDateModule, MatRippleModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSliderModule } from '@angular/material/slider';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTreeModule } from '@angular/material/tree';
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
import { ReviewQuestionComponent } from './review-question/review-question.component';
import { ReviewGameComponent } from './review-game/review-game.component';
import { ErrorService } from 'src/services/error.service';
import { SignalrService } from 'src/services/signalr.service';

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
    AnswerQuestionComponent,
    ReviewQuestionComponent,
    ReviewGameComponent
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
], { relativeLinkResolution: 'legacy' }),
    BrowserAnimationsModule,
    CookieModule.forRoot()
  ],
  providers: [
    AuthGuardService,
    LoginService,
    ErrorService,
    SignalrService,
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
