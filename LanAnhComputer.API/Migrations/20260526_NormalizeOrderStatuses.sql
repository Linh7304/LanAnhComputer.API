UPDATE Orders
SET PaymentStatus = 'Pending'
WHERE PaymentStatus IS NULL
   OR UPPER(PaymentStatus) IN ('PENDING', 'UNPAID');

UPDATE Orders
SET PaymentStatus = 'Paid'
WHERE UPPER(PaymentStatus) = 'PAID';

UPDATE Orders
SET PaymentStatus = 'Cancelled'
WHERE UPPER(PaymentStatus) = 'CANCELLED';

UPDATE Orders
SET OrderStatus = 'Pending'
WHERE OrderStatus IS NULL
   OR UPPER(OrderStatus) IN ('PENDING', 'PROCESSING');

UPDATE Orders
SET OrderStatus = 'Shipped'
WHERE UPPER(OrderStatus) = 'SHIPPED';

UPDATE Orders
SET OrderStatus = 'Delivered'
WHERE UPPER(OrderStatus) = 'DELIVERED';

UPDATE Orders
SET OrderStatus = 'Cancelled'
WHERE UPPER(OrderStatus) = 'CANCELLED';
