import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { ReactiveFormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';

@NgModule({
	imports: [MatFormFieldModule, MatInputModule, MatButtonModule, ReactiveFormsModule],
})
export class AppModule {}
