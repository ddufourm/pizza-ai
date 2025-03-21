import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
	let component: AppComponent;
	let fixture: ComponentFixture<AppComponent>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			declarations: [AppComponent],
			imports: [ReactiveFormsModule],
		}).compileComponents();
	});

	beforeEach(() => {
		fixture = TestBed.createComponent(AppComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	// Test pour vérifier que le composant est créé
	it('should create the component', () => {
		expect(component).toBeTruthy();
	});

	// Test pour vérifier la validité initiale du formulaire
	it('should have an invalid form when initialized', () => {
		expect(component.form.valid).toBeFalse();
	});

	// Test pour vérifier la validation du champ textInput
	it('should validate textInput correctly', () => {
		const textInput = component.form.controls['textInput'];
		textInput.setValue('Une humeur joyeuse');
		expect(textInput.valid).toBeTrue(); // Le champ est valide si la longueur est <= 30 caractères

		textInput.setValue('Une humeur très joyeuse et accompagnée de beaucoup de mots');
		expect(textInput.valid).toBeFalse(); // Le champ est invalide si la longueur dépasse 30 caractères
	});

	// Test pour vérifier le comportement lors de la soumission du formulaire
	it('should call onSubmit when the form is submitted', () => {
		spyOn(component, 'onSubmit');
		const formElement = fixture.nativeElement.querySelector('form');
		formElement.dispatchEvent(new Event('submit'));
		expect(component.onSubmit).toHaveBeenCalled();
	});

	// Test pour vérifier le rendu du titre dans le HTML
	it('should render the title in the template', () => {
		const compiled = fixture.nativeElement as HTMLElement;
		expect(compiled.querySelector('h2')?.textContent).toContain(
			'Décrivez votre humeur pour obtenir une suggestion de pizza'
		);
	});
});
