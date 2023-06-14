#!/bin/sh
if ! command -v sqlcmd &> /dev/null; then
  # Install the SqlServer module
  sudo apt-get install mssql-tools
fi

sql_files=$(ls ./migration-scripts/*.sql 2>/dev/null)
echo "$sql_files"

server=$DB_SERVER
username=$DB_USERNAME
database=$DB_NAME
password=$DB_SQLPASSWD

for file in $sql_files; do
/opt/mssql-tools/bin/sqlcmd -S "$server" -U "$username" -P "$password" -d "$database" -i "$file"
done
