-- Reference design for the database.
-- The live schema is created by the EF Core migration in
-- server/src/Ecommerce.Infrastructure/Migrations, which already matches most of this.
-- Still missing there: CHECK constraints, varchar lengths, unique constraints on
-- category name/slug, password_reset_tokens.token_hash and the Stripe ids on orders,
-- the set_updated_at() triggers, and the email_logs table.

CREATE TABLE users (
  id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  email         varchar(255) NOT NULL UNIQUE,
  password_hash varchar(255) NOT NULL,
  first_name    varchar(100),
  last_name     varchar(100),
  created_at    timestamptz NOT NULL DEFAULT now(),
  updated_at    timestamptz NOT NULL DEFAULT now()
);

CREATE FUNCTION set_updated_at() RETURNS trigger AS $$
BEGIN
  NEW.updated_at = now();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER users_updated_at
  BEFORE UPDATE ON users
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE password_reset_tokens (
  id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id    uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token_hash varchar(255) NOT NULL UNIQUE,
  expires_at timestamptz NOT NULL,
  used_at    timestamptz,
  created_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_password_reset_tokens_user_id
  ON password_reset_tokens(user_id);

CREATE TABLE categories (
  id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  name       varchar(100) NOT NULL UNIQUE
             CONSTRAINT categories_name_allowed
             CHECK (name IN ('Electronics','Books','Kitchen','Stationery','Toys')),
  slug       varchar(100) NOT NULL UNIQUE,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TRIGGER categories_updated_at
  BEFORE UPDATE ON categories
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE products (
  id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  category_id    uuid NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
  title          varchar(255) NOT NULL,
  description    text,
  price          decimal(10,2) NOT NULL,
  stock_quantity int NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
  image_url      varchar(500),
  is_active      boolean NOT NULL DEFAULT true,
  created_at     timestamptz NOT NULL DEFAULT now(),
  updated_at     timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_products_category_id ON products(category_id);

CREATE TRIGGER products_updated_at
  BEFORE UPDATE ON products
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE carts (
  id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id    uuid UNIQUE REFERENCES users(id) ON DELETE CASCADE,
  session_id varchar(255) UNIQUE,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  CONSTRAINT carts_owner_required
    CHECK (user_id IS NOT NULL OR session_id IS NOT NULL)
);

CREATE INDEX idx_carts_session_id ON carts(session_id);

CREATE TRIGGER carts_updated_at
  BEFORE UPDATE ON carts
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE cart_items (
  id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  cart_id    uuid NOT NULL REFERENCES carts(id) ON DELETE CASCADE,
  product_id uuid NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
  quantity   int NOT NULL CHECK (quantity > 0),
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  CONSTRAINT cart_items_unique_product UNIQUE (cart_id, product_id)
);

CREATE INDEX idx_cart_items_cart_id ON cart_items(cart_id);

CREATE TRIGGER cart_items_updated_at
  BEFORE UPDATE ON cart_items
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TYPE order_status AS ENUM (
  'pending',
  'paid',
  'failed',
  'cancelled',
  'refunded',
  'partially_refunded',
  'disputed'
);

CREATE TYPE fulfillment_status AS ENUM (
  'unfulfilled',
  'processing',
  'shipped',
  'delivered',
  'cancelled'
);

CREATE TABLE orders (
  id                       uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id                  uuid NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
  status                   order_status NOT NULL DEFAULT 'pending',
  fulfillment_status       fulfillment_status NOT NULL DEFAULT 'unfulfilled',
  total_amount             decimal(10,2) NOT NULL CHECK (total_amount >= 0),
  shipping_name            varchar(200) NOT NULL,
  shipping_phone           varchar(30),
  shipping_line1           varchar(255) NOT NULL,
  shipping_line2           varchar(255),
  shipping_city            varchar(100) NOT NULL,
  shipping_state           varchar(100),
  shipping_postal_code     varchar(20) NOT NULL,
  shipping_country         char(2) NOT NULL,
  stripe_session_id        varchar(255) UNIQUE,
  stripe_payment_intent_id varchar(255) UNIQUE,
  refunded_amount          decimal(10,2) NOT NULL DEFAULT 0
                           CHECK (refunded_amount >= 0),
  failure_reason           text,
  paid_at                  timestamptz,
  created_at               timestamptz NOT NULL DEFAULT now(),
  updated_at               timestamptz NOT NULL DEFAULT now(),
  CONSTRAINT orders_refund_within_total
    CHECK (refunded_amount <= total_amount)
);

CREATE INDEX idx_orders_user_id ON orders(user_id);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_fulfillment_status ON orders(fulfillment_status);

CREATE TRIGGER orders_updated_at
  BEFORE UPDATE ON orders
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE order_items (
  id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  order_id      uuid NOT NULL REFERENCES orders(id) ON DELETE RESTRICT,
  product_id    uuid NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
  product_title varchar(255) NOT NULL,
  quantity      int NOT NULL CHECK (quantity > 0),
  unit_price    decimal(10,2) NOT NULL CHECK (unit_price >= 0),
  created_at    timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_order_items_order_id ON order_items(order_id);

CREATE TABLE stripe_events (
  id           varchar(255) PRIMARY KEY,
  event_type   varchar(100) NOT NULL,
  processed_at timestamptz NOT NULL DEFAULT now()
);

CREATE TYPE email_status AS ENUM (
  'sent',
  'delivered',
  'failed',
  'bounced'
);

CREATE TABLE email_logs (
  id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  order_id    uuid REFERENCES orders(id) ON DELETE CASCADE,
  user_id     uuid REFERENCES users(id) ON DELETE CASCADE,
  email_type  varchar(50) NOT NULL,
  status      email_status NOT NULL DEFAULT 'sent',
  provider_id varchar(255) UNIQUE,
  error       text,
  created_at  timestamptz NOT NULL DEFAULT now(),
  updated_at  timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX idx_email_logs_order_id ON email_logs(order_id);

CREATE TRIGGER email_logs_updated_at
  BEFORE UPDATE ON email_logs
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();
