import mysql.connector
import os
from dotenv import load_dotenv

# .env dosyası varsa otomatik yükle (yoksa environment variable'lardan okur)
load_dotenv()

def get_db():
    """
    Veritabanı bağlantısını döndürür.
    Şifre .env dosyasından veya environment variable'dan okunur.

    Kurulum:
        1. .env.example dosyasını .env olarak kopyalayın
        2. Gerçek değerleri doldurun
        3. .env dosyasını asla git'e commit etmeyin!
    """
    ssl_enabled = os.environ.get("DB_SSL", "false").lower() == "true"

    return mysql.connector.connect(
        host         = os.environ.get("DB_HOST",     "localhost"),
        port         = int(os.environ.get("DB_PORT", "3306")),
        user         = os.environ.get("DB_USER",     "root"),
        password     = os.environ.get("DB_PASSWORD", ""),
        database     = os.environ.get("DB_NAME",     "bibliorate"),
        ssl_disabled = not ssl_enabled
    )
