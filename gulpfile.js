var gulp = require('gulp'),
    cssmin = require("gulp-cssmin")
rename = require("gulp-rename");
const sass = require('gulp-sass')(require('sass'));

gulp.task('default', function () {
    return gulp.src('assets/scss/site.scss')  //biên dịch file scss thành css
        .pipe(sass().on('error', sass.logError))
        //.pipe(cssmin())
        .pipe(rename({
            //suffix: ".min"   
        }))
        .pipe(gulp.dest('wwwroot/css/')); //kết quả lưu ở wwwroot/css
});

gulp.task("watch", function() {
    gulp.watch("assets/scss/*.scss", gulp.series('default'));  //mỗi khi file scss có thay đổi thì thực hiện tác vụ default
});