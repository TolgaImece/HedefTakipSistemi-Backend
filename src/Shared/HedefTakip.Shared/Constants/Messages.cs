namespace HedefTakip.Shared.Constants;

public static class Messages
{
    public static class Auth
    {
        public const string InvalidCredentials      = "AUTH_INVALID_CREDENTIALS";
        public const string InvalidCredentialsUser  = "E-posta veya şifreniz hatalı.";

        public const string InvalidRefreshToken     = "AUTH_INVALID_REFRESH_TOKEN";
        public const string InvalidRefreshTokenUser = "Oturumunuz sona erdi, lütfen tekrar giriş yapınız.";

        public const string Forbidden               = "AUTH_FORBIDDEN";
        public const string ForbiddenUser           = "Bu işlem için yetkiniz bulunmamaktadır.";

        public const string LoginSuccess            = "Giriş başarılı.";
        public const string LogoutSuccess           = "Çıkış başarılı.";
        public const string PasswordChanged         = "Şifre başarıyla değiştirildi.";

        public const string WrongPassword           = "AUTH_WRONG_PASSWORD";
        public const string WrongPasswordUser       = "Mevcut şifreniz hatalı.";
    }

    public static class User
    {
        public const string NotFound      = "USER_NOT_FOUND";
        public const string NotFoundUser  = "Kullanıcı bulunamadı.";

        public const string AlreadyExists     = "USER_ALREADY_EXISTS";
        public const string AlreadyExistsUser = "Bu e-posta adresi zaten kullanılıyor.";

        public const string InvalidRole     = "USER_INVALID_ROLE";
        public const string InvalidRoleUser = "Geçersiz kullanıcı rolü.";

        public const string Created = "Kullanıcı başarıyla oluşturuldu.";
        public const string Updated = "Kullanıcı başarıyla güncellendi.";
        public const string Deleted = "Kullanıcı başarıyla silindi.";
    }

    public static class Department
    {
        public const string NotFound     = "DEPARTMENT_NOT_FOUND";
        public const string NotFoundUser = "Departman bulunamadı.";

        public const string AlreadyExists     = "DEPARTMENT_ALREADY_EXISTS";
        public const string AlreadyExistsUser = "Bu departman adı zaten kullanılıyor.";

        public const string Created = "Departman başarıyla oluşturuldu.";
        public const string Updated = "Departman başarıyla güncellendi.";
        public const string Deleted = "Departman başarıyla silindi.";
    }

    public static class Position
    {
        public const string NotFound     = "POSITION_NOT_FOUND";
        public const string NotFoundUser = "Pozisyon bulunamadı.";

        public const string Created = "Pozisyon başarıyla oluşturuldu.";
        public const string Updated = "Pozisyon başarıyla güncellendi.";
        public const string Deleted = "Pozisyon başarıyla silindi.";
    }

    public static class Period
    {
        public const string NotFound     = "PERIOD_NOT_FOUND";
        public const string NotFoundUser = "Dönem bulunamadı.";

        public const string AlreadyExists     = "PERIOD_ALREADY_EXISTS";
        public const string AlreadyExistsUser = "Bu dönem zaten mevcut.";

        public const string AlreadyClosed     = "PERIOD_ALREADY_CLOSED";
        public const string AlreadyClosedUser = "Bu dönem zaten kapatılmış.";

        public const string Created = "Dönem başarıyla oluşturuldu.";
        public const string Updated = "Dönem başarıyla güncellendi.";
        public const string Deleted = "Dönem başarıyla silindi.";
        public const string Closed  = "Dönem başarıyla kapatıldı.";
    }

    public static class GoalCategory
    {
        public const string NotFound     = "GOAL_CATEGORY_NOT_FOUND";
        public const string NotFoundUser = "Hedef kategorisi bulunamadı.";

        public const string AlreadyExists     = "GOAL_CATEGORY_ALREADY_EXISTS";
        public const string AlreadyExistsUser = "Bu kategori adı zaten kullanılıyor.";

        public const string Created = "Kategori başarıyla oluşturuldu.";
        public const string Updated = "Kategori başarıyla güncellendi.";
        public const string Deleted = "Kategori başarıyla silindi.";
    }

    public static class GoalTemplate
    {
        public const string NotFound     = "GOAL_TEMPLATE_NOT_FOUND";
        public const string NotFoundUser = "Hedef şablonu bulunamadı.";

        public const string Created = "Hedef şablonu başarıyla oluşturuldu.";
        public const string Updated = "Hedef şablonu başarıyla güncellendi.";
        public const string Deleted = "Hedef şablonu başarıyla silindi.";
    }

    public static class GoalAssignment
    {
        public const string NotFound     = "GOAL_ASSIGNMENT_NOT_FOUND";
        public const string NotFoundUser = "Hedef ataması bulunamadı.";

        public const string InvalidStatus     = "GOAL_ASSIGNMENT_INVALID_STATUS";
        public const string InvalidStatusUser = "Geçersiz hedef durumu.";

        public const string Created       = "Hedef başarıyla atandı.";
        public const string StatusUpdated = "Hedef durumu başarıyla güncellendi.";
        public const string Deleted       = "Hedef ataması başarıyla silindi.";
    }

    public static class Goal
    {
        public const string NotFound     = "GOAL_NOT_FOUND";
        public const string NotFoundUser = "Hedef bulunamadı.";

        public const string AlreadyExists     = "GOAL_ALREADY_EXISTS";
        public const string AlreadyExistsUser = "Bu hedef zaten mevcut.";

        public const string Created  = "Hedef başarıyla oluşturuldu.";
        public const string Updated  = "Hedef başarıyla güncellendi.";
        public const string Deleted  = "Hedef başarıyla silindi.";
        public const string Assigned = "Hedef başarıyla atandı.";
    }

    public static class Comment
    {
        public const string NotFound     = "COMMENT_NOT_FOUND";
        public const string NotFoundUser = "Yorum bulunamadı.";

        public const string Created = "Yorum başarıyla eklendi.";
        public const string Deleted = "Yorum başarıyla silindi.";
    }

    public static class General
    {
        public const string UnexpectedError     = "UNEXPECTED_ERROR";
        public const string UnexpectedErrorUser = "Beklenmeyen bir hata oluştu, lütfen tekrar deneyiniz.";

        public const string ValidationError     = "VALIDATION_ERROR";
        public const string ValidationErrorUser = "Lütfen girilen bilgileri kontrol ediniz.";
    }
}
