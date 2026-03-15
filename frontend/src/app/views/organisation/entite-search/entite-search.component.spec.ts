import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EntiteSearchComponent } from './entite-search.component';

describe('EntiteSearchComponent', () => {
  let component: EntiteSearchComponent;
  let fixture: ComponentFixture<EntiteSearchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EntiteSearchComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EntiteSearchComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
