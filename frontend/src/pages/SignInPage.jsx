import { useState } from 'react'
import './SignInPage.css'

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/+$/, '')

function BrandMark() {
  return (
    <span className="signin-brand-mark" aria-hidden="true">
      <svg viewBox="0 0 24 24" fill="none">
        <path d="M12 5v14M5 12h14" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" />
      </svg>
    </span>
  )
}

function SignInPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [signedInUser, setSignedInUser] = useState(null)

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)

    try {
      const response = await fetch(`${apiBaseUrl}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      })
      const result = await response.json()

      if (!response.ok) {
        throw new Error(result.detail || result.title || 'Unable to sign in. Please check your details and try again.')
      }

      setSignedInUser(result.user)
    } catch (requestError) {
      setError(
        requestError instanceof TypeError
          ? 'We couldn’t connect to Cliniq. Check your connection and try again.'
          : requestError.message,
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  function signOut() {
    setSignedInUser(null)
    setPassword('')
    setError('')
  }

  return (
    <main className="signin-page">
      <section className="signin-story" aria-label="About Cliniq">
        <a className="signin-brand" href="/" aria-label="Cliniq home">
          <BrandMark />
          <span>cliniq<span className="signin-brand-period">.</span></span>
        </a>

        <div className="signin-story-copy">
          <span className="signin-overline"><span /> YOUR CLINIC, IN GOOD HANDS</span>
          <h1>More time for<br />what matters.</h1>
          <p>A thoughtful workspace for your team to make every visit feel a little easier.</p>

          <div className="signin-preview" aria-hidden="true">
            <div className="preview-heading">
              <span className="preview-heading-icon"><BrandMark /></span>
              <span><strong>Today at your clinic</strong><small>Your day, at a glance</small></span>
              <span className="preview-dots">···</span>
            </div>
            <div className="preview-divider" />
            <div className="preview-appointment">
              <span className="preview-time">09:30</span>
              <span className="preview-timeline"><i /></span>
              <span className="preview-patient"><strong>Morning appointments</strong><small>Care starts with a warm welcome</small></span>
              <span className="preview-count">08</span>
            </div>
            <div className="preview-footer"><span><i /> All set for today</span><span>CLINIQ WORKSPACE</span></div>
          </div>
        </div>

        <div className="signin-story-footer">
          <span>Care, made simpler.</span>
          <span>Secure access for your clinic team</span>
        </div>
      </section>

      <section className="signin-panel">
        <div className="signin-form-wrap">
          {signedInUser ? (
            <div className="signin-complete" role="status">
              <span className="signin-success-mark" aria-hidden="true">
                <svg viewBox="0 0 24 24" fill="none">
                  <path d="m6 12 4 4 8-8" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
              </span>
              <span className="signin-kicker">SIGN IN COMPLETE</span>
              <h2>Welcome, {signedInUser.fullName}.</h2>
              <p>You’re signed in to Cliniq as a <strong>{signedInUser.role}</strong>. Your clinic workspace is ready for you.</p>
              <button className="signin-secondary-button" type="button" onClick={signOut}>Sign out</button>
            </div>
          ) : (
            <>
              <div className="signin-mobile-brand">
                <BrandMark />
                <span>cliniq<span className="signin-brand-period">.</span></span>
              </div>
              <span className="signin-kicker">WELCOME BACK</span>
              <h2>Sign in to Cliniq</h2>
              <p className="signin-description">Use your clinic account to continue to your workspace.</p>

              <form className="signin-form" onSubmit={handleSubmit}>
                <label htmlFor="signin-email">Email address</label>
                <input
                  autoComplete="username"
                  id="signin-email"
                  name="email"
                  type="email"
                  placeholder="you@yourclinic.com"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  required
                />

                <div className="signin-password-label">
                  <label htmlFor="signin-password">Password</label>
                  <span>Need help? Contact your clinic administrator.</span>
                </div>
                <div className="signin-password-field">
                  <input
                    autoComplete="current-password"
                    id="signin-password"
                    name="password"
                    type={showPassword ? 'text' : 'password'}
                    placeholder="Enter your password"
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                    required
                  />
                  <button
                    className="signin-password-toggle"
                    type="button"
                    onClick={() => setShowPassword((visible) => !visible)}
                    aria-label={showPassword ? 'Hide password' : 'Show password'}
                    aria-pressed={showPassword}
                  >
                    {showPassword ? (
                      <svg viewBox="0 0 24 24" fill="none" aria-hidden="true">
                        <path d="M3 3 21 21M10.6 10.7a2 2 0 0 0 2.7 2.7M9.9 5.2A10.7 10.7 0 0 1 12 5c5.2 0 8.5 4.8 9.5 6.5a1 1 0 0 1 0 1A14 14 0 0 1 18 16M6.2 6.3A15 15 0 0 0 2.5 11.5a1 1 0 0 0 0 1C3.5 14.2 6.8 19 12 19c.9 0 1.8-.2 2.6-.5" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" />
                      </svg>
                    ) : (
                      <svg viewBox="0 0 24 24" fill="none" aria-hidden="true">
                        <path d="M2.5 12s3.3-7 9.5-7 9.5 7 9.5 7-3.3 7-9.5 7-9.5-7-9.5-7Z" stroke="currentColor" strokeWidth="1.7" strokeLinejoin="round" />
                        <circle cx="12" cy="12" r="3" stroke="currentColor" strokeWidth="1.7" />
                      </svg>
                    )}
                  </button>
                </div>

                {error && <p className="signin-error" role="alert">{error}</p>}

                <button className="signin-submit" type="submit" disabled={isSubmitting}>
                  {isSubmitting ? 'Signing in…' : 'Sign in'}
                  {!isSubmitting && (
                    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true">
                      <path d="M5 12h14m-6-6 6 6-6 6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                  )}
                </button>
              </form>

              <div className="signin-security-note">
                <svg viewBox="0 0 24 24" fill="none" aria-hidden="true">
                  <path d="M12 3 5 6v5c0 4.6 2.9 8.1 7 10 4.1-1.9 7-5.4 7-10V6l-7-3Z" stroke="currentColor" strokeWidth="1.6" strokeLinejoin="round" />
                  <path d="m9 12 2 2 4-4" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
                <span>Your account is protected with secure sign in.</span>
              </div>
            </>
          )}
        </div>
        <footer className="signin-panel-footer">
          <span>© Cliniq</span>
          <span>Made for better care</span>
        </footer>
      </section>
    </main>
  )
}

export default SignInPage
