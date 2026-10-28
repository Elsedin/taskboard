import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient()],
    });
    service = TestBed.inject(AuthService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('reports not logged in when there is no token', () => {
    expect(service.isLoggedIn()).toBe(false);
  });

  it('reports logged in when a token exists', () => {
    localStorage.setItem('taskboard_token', 'fake-token');
    expect(service.isLoggedIn()).toBe(true);
  });

  it('returns stored email', () => {
    localStorage.setItem('taskboard_email', 'demo@example.com');
    expect(service.getEmail()).toBe('demo@example.com');
  });

  it('logout clears token and email', () => {
    localStorage.setItem('taskboard_token', 'fake-token');
    localStorage.setItem('taskboard_email', 'demo@example.com');

    service.logout();

    expect(service.getToken()).toBeNull();
    expect(service.getEmail()).toBeNull();
    expect(service.isLoggedIn()).toBe(false);
  });
});
