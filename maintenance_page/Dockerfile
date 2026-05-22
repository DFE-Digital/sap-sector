FROM nginxinc/nginx-unprivileged:1.29-alpine

COPY ./maintenance_page/index.html /usr/share/nginx/html/index.html
COPY ./maintenance_page/nginx.conf /etc/nginx/conf.d/default.conf
COPY ./SAPSec.Web/wwwroot/css/govuk-frontend.min.css /usr/share/nginx/html/css/govuk-frontend.min.css
COPY ./SAPSec.Web/wwwroot/css/dfefrontend.css /usr/share/nginx/html/css/dfefrontend.css
COPY ./SAPSec.Web/wwwroot/assets /usr/share/nginx/html/assets

EXPOSE 8080
