    INSERT INTO dbo.[user] (
    identification_number,
    email,
    first_name,
    last_name_one,
    last_name_two,
    type,
    password_hash,
    password_salt
)
VALUES (
    '132456789',
    'operator@snoopyairlines.com',
    'TestFN',
    'TestLN',
    'TestLNT',
    'OP',
    '$2a$11$wVAmBSnPwpHtaVrz/NBaAeDcFXYKW0QigNIpnc/0l8w/C6lLxi8m.',
    ''
)