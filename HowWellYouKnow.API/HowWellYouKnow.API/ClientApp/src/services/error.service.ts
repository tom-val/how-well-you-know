import { HttpErrorResponse } from '@angular/common/http';
import {  Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiError } from 'src/app/dtos/api-error';

@Injectable()
export class ErrorService {
  constructor(private _snackBar: MatSnackBar) {}

  showError(errorResponse: HttpErrorResponse): void {
    const errorData = errorResponse.error as ApiError;
    this._snackBar.open(errorData.message, 'Close');
  }
}
