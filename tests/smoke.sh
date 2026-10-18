#!/usr/bin/env bash
set -uo pipefail

BASE="${BASE_URL:-http://127.0.0.1:8091}"
JAR="$(mktemp)"
FAILURES=0

csrf() {
  curl -s -b "$JAR" -c "$JAR" "$BASE$1" | grep -o 'name="__RequestVerificationToken" type="hidden" value="[^"]*"' | head -1 | sed 's/.*value="//;s/"//'
}

login() {
  : > "$JAR"
  local token
  token="$(csrf /login)"
  curl -s -o /dev/null -b "$JAR" -c "$JAR" -d "__RequestVerificationToken=$token" --data-urlencode "email=$1" --data-urlencode "password=$2" "$BASE/login"
}

expect() {
  local want="$1" path="$2" got
  got="$(curl -s -o /dev/null -w '%{http_code}' -b "$JAR" -c "$JAR" "$BASE$path")"
  if [ "$got" = "200" ] && curl -s -b "$JAR" -c "$JAR" "$BASE$path" | grep -qiE 'Exception|Stack Trace|Unhandled exception'; then got="200+error-in-body"; fi
  if [ "$got" = "$want" ]; then
    printf 'ok    %s %s\n' "$got" "$path"
  else
    printf 'FAIL  %s (wanted %s) %s\n' "$got" "$want" "$path"
    FAILURES=$((FAILURES + 1))
  fi
}

check() {
  local label="$1"
  shift

  if "$@"; then
    printf 'ok    %s\n' "$label"
    return 0
  fi

  printf 'FAIL  %s\n' "$label"
  FAILURES=$((FAILURES + 1))
  return 1
}

every_cookie_has() {
  [ -n "$1" ] && ! printf '%s\n' "$1" | grep -viqE ";[[:space:]]*$2([[:space:];]|\$)"
}

session_cookie() {
  local cookies names
  cookies="$(curl -s -o /dev/null -D - "$BASE/login" | tr -d '\r' | grep -i '^set-cookie:' | sed 's/^[^:]*: *//')"
  names="$(printf '%s\n' "$cookies" | cut -d= -f1 | sort -u)"

  check "/login sets a cookie" test -n "$cookies" || return
  check "/login sets only __session (found: ${names//$'\n'/, })" test "$names" = "__session" || return
  check "__session is HttpOnly" every_cookie_has "$cookies" httponly
  check "__session is SameSite=Lax" every_cookie_has "$cookies" samesite=lax

  case "$BASE" in
    https://*) check "__session is Secure" every_cookie_has "$cookies" secure ;;
  esac
}

rejected_login() {
  : > "$JAR"
  local token got
  token="$(csrf /login)"
  got="$(curl -s -o /dev/null -w '%{http_code}' -b "$JAR" -c "$JAR" -d "__RequestVerificationToken=$token" --data-urlencode "email=smoke@flyvip.invalid" --data-urlencode "password=Not-The-Password-2026" "$BASE/login")"

  check "POST /login with a wrong password re-renders the form (got $got)" test "$got" = "200"
}

wait_for_health() {
  local deadline=$((SECONDS + 60))

  while [ "$SECONDS" -lt "$deadline" ]; do
    [ "$(curl -s -m 2 -o /dev/null -w '%{http_code}' "$BASE/health")" = "200" ] && return 0
    sleep 1
  done

  return 1
}

restart() {
  echo "== restart"
  login ana.personal@flyvip.test 'FlyVIP-Personal-2026!'
  expect 200 /portal

  check "restart command succeeds" bash -c "$SMOKE_RESTART" || return
  check "/health answers 200 within 60s" wait_for_health || return
  expect 200 /portal
}

finish() {
  rm -f "$JAR"

  if [ "$FAILURES" -gt 0 ]; then
    echo "$FAILURES failure(s)"
    exit 1
  fi

  echo "ALL OK"
  exit 0
}

echo "== anonymous"
: > "$JAR"
expect 200 /
expect 200 /health
expect 200 /login
expect 302 /admin
expect 302 /portal
session_cookie
rejected_login

if [ "${SMOKE_ANONYMOUS:-}" = "1" ]; then
  finish
fi

echo "== admin"
login admin@flyvip.test 'FlyVIP-Admin-2026!'
for path in /admin /admin/members /admin/members/new /admin/members/2 /admin/members/2/edit /admin/payments /admin/payments/new '/admin/payments/new?user_id=2' /admin/points /admin/points/adjust /admin/points/transfer /admin/reservations /admin/reservations/new /admin/reservations/1 /admin/aircrafts /admin/aircrafts/new /admin/aircrafts/1/edit /admin/aircraft-types /admin/aircraft-types/new /admin/maintenance /admin/maintenance/new /admin/airports /admin/airports/new /admin/routes /admin/routes/new /admin/routes/1/edit /admin/pilots /admin/pilots/new /admin/pilots/1/edit /admin/pilot-documents /admin/pilot-documents/new /admin/pilot-certifications /admin/pilot-certifications/new /admin/flight-schedule /admin/flight-schedule/new; do
  expect 200 "$path"
done
expect 302 /portal

echo "== member (personal)"
login ana.personal@flyvip.test 'FlyVIP-Personal-2026!'
for path in /portal /portal/points /portal/profile /portal/reservations /portal/reservations/new; do
  expect 200 "$path"
done
expect 302 /admin
expect 302 /admin/members

echo "== sub-member"
login lucia.family@flyvip.test 'FlyVIP-Submember-2026!'
for path in /portal /portal/points /portal/profile /portal/reservations; do
  expect 200 "$path"
done
expect 302 /admin

echo "== banned"
login banned.member@flyvip.test 'FlyVIP-Banned-2026!'
expect 302 /portal

echo "== sign out"
login ana.personal@flyvip.test 'FlyVIP-Personal-2026!'
token="$(csrf /portal)"
got="$(curl -s -o /dev/null -w '%{http_code}' -b "$JAR" -c "$JAR" -d "__RequestVerificationToken=$token" "$BASE/logout")"
[ "$got" = "302" ] || [ "$got" = "303" ] && echo "ok    $got POST /logout" || { echo "FAIL  $got POST /logout"; FAILURES=$((FAILURES + 1)); }
expect 302 /portal

if [ -n "${SMOKE_RESTART:-}" ]; then
  restart
fi

finish
