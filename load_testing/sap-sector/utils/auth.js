// Session-cookie reuse for authenticated load testing against a real
// DSI-protected environment (test/review), as an alternative to the
// LoadTest-mode auto-auth bypass (which only works on a locally-run instance).
//
// DfE Sign-in has MFA, so the login itself can't be scripted. Instead:
//   1. A human signs in once through a real browser, using a dedicated
//      test/service DSI account (never a real school leader's account),
//      completing MFA as normal.
//   2. Copy the "SAPSec.Auth" cookie's value from DevTools
//      (Application/Storage > Cookies) after signing in.
//   3. Set it as SESSION_COOKIE in .env (never commit it, never paste it
//      into chat/logs - it's a live credential, treat it like a password).
//
// The cookie is HttpOnly (browsers won't expose it to page JS) but that
// doesn't stop it being read manually from DevTools or sent as a raw HTTP
// header by a non-browser client like k6.
//
// The session uses sliding expiration, so it stays alive for the length of
// a load test run as long as requests keep flowing - but it WILL eventually
// expire and need to be refreshed (repeat steps 1-3) for a new run.

const COOKIE_NAME = 'SAPSec.Auth'

export function hasSessionCookie () {
  return Boolean(__ENV.SESSION_COOKIE)
}

export function authParams (extraParams = {}) {
  if (!hasSessionCookie()) {
    return extraParams
  }

  return {
    ...extraParams,
    headers: {
      ...(extraParams.headers || {}),
      Cookie: `${COOKIE_NAME}=${__ENV.SESSION_COOKIE}`
    }
  }
}
