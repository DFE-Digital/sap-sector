/* eslint-disable @typescript-eslint/no-require-imports */
const gulp = require("gulp");
const sass = require("gulp-dart-sass");
const async = require("async");
const cleanCSS = require("gulp-clean-css");
const sourcemaps = require("gulp-sourcemaps");
const rename = require("gulp-rename");

const buildSass = () =>
    gulp
        .src("AssetSrc/scss/*.scss")
        .pipe(sourcemaps.init())
        .pipe(
            sass({
                logger: {
                    warn: (message, options) => {
                        const filePath = options?.span?.url || options?.span?.context?.file || '';
                        if (!String(filePath).includes('govuk-frontend')) {
                            // If it's NOT from govuk-frontend, output the warning.
                            console.warn(message);
                        }
                        // If it is from govuk-frontend suppressed
                    }
                }
            }).on("error", sass.logError)
        )
        .pipe(cleanCSS())
        .pipe(sourcemaps.write("./"))
        .pipe(gulp.dest("wwwroot/css"));

const copyStaticAssets = () =>
  gulp
    .src(["node_modules/govuk-frontend/dist/govuk/assets/**/*"], {
      encoding: false,
    })
    .pipe(gulp.dest("wwwroot/assets"))
    .on("end", () =>
      gulp
        .src(
          ["node_modules/govuk-frontend/dist/govuk/assets/images/favicon.ico"],
          { encoding: false }
        )
        .pipe(gulp.dest("wwwroot/"))
    )
    .on("end", () =>
      gulp
        .src(
          [
            "node_modules/govuk-frontend/dist/govuk/assets/images/govuk-icon-180.png",
          ],
          { encoding: false }
        )
        .pipe(rename("apple-touch-icon.png"))
        .pipe(gulp.dest("wwwroot/"))
        .pipe(rename("apple-touch-icon-120x120.png"))
        .pipe(gulp.dest("wwwroot/"))
        .pipe(rename("apple-touch-icon-precomposed.png"))
        .pipe(gulp.dest("wwwroot/"))
    )
    .on("end", () =>
      gulp
        .src(["node_modules/govuk-frontend/dist/govuk/govuk-frontend.min.js*"])
        .pipe(gulp.dest("wwwroot/js/"))
    )
    .on("end", () =>
      gulp
        .src(["node_modules/govuk-frontend/dist/govuk/govuk-frontend.min.css"])
        .pipe(gulp.dest("wwwroot/css/"))
    )
    .on("end", () =>
      gulp
        .src([
          "node_modules/lodash.debounce/index.js",
        ])
        .pipe(gulp.dest("wwwroot/js/lodash.debounce/"))
    )
    .on("end", () =>
      gulp
        .src([
          "node_modules/accessible-autocomplete/dist/accessible-autocomplete.min.js*",
        ])
        .pipe(gulp.dest("wwwroot/js/"))
    )
    .on("end", () =>
      gulp
        .src([
            "node_modules/accessible-autocomplete/dist/accessible-autocomplete.min.css",
        ])
        .pipe(gulp.dest("wwwroot/css/"))
    )
    .on("end", () =>
      gulp
        .src(["node_modules/dfe-frontend/dist/dfefrontend.js"])
        .pipe(gulp.dest("wwwroot/js/"))
    )
    .on("end", () =>
      gulp
        .src(["node_modules/dfe-frontend/dist/dfefrontend.css"])
        .pipe(gulp.dest("wwwroot/css/"))
    )
    .on("end", () =>
       gulp
         .src(["node_modules/@ministryofjustice/frontend/moj/moj-frontend.min.js"])
         .pipe(gulp.dest("wwwroot/js/"))
    )
    .on("end", () =>
        gulp
         .src(["node_modules/@ministryofjustice/frontend/moj/moj-frontend.min.css"])
          .pipe(gulp.dest("wwwroot/css/"))
    )
    .on("end", () =>
        gulp
         .src(["node_modules/@ministryofjustice/frontend/moj/assets/**/*"], { encoding: false })
         .pipe(gulp.dest("wwwroot/assets/"))
    )
    .on("end", () =>
      gulp
        .src(["AssetSrc/images/*"], { encoding: false })
        .pipe(gulp.dest("wwwroot/assets/images"))
    )
    .on("end", () =>
      gulp
        .src(["AssetSrc/js/*"], { encoding: false })
        .pipe(gulp.dest("wwwroot/js/"))
    )
    .on("end", () => 
        gulp
        .src(["AssetSrc/leaflet/*"], { encoding: false })
        .pipe(gulp.dest("wwwroot/assets/leaflet/"))
    );

gulp.task("build-fe", () => {
  return async.series([
    (next) => buildSass().on("end", next),
    (next) => copyStaticAssets().on("end", next)
  ]);
});
