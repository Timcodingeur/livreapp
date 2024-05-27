export const UserModel = (sequelize, DataTypes) => {
  return sequelize.define("User", {
    id: {
      type: DataTypes.INTEGER,
      primaryKey: true,
      autoIncrement: true,
    },
    username: {
      type: DataTypes.STRING,
      allowNull: false,
      unique: {
        msg: "Ce username est déjà pris.",
      },
      notEmpty: {
        msg: "Le prénom ne peut pas être vide.",
      },
      notNull: {
        msg: "Le prénom est une propriété obligatoire",
      },
    },
    password: {
      type: DataTypes.STRING,
      allowNull: false,
    },
    firstname: {
      type: DataTypes.STRING,
      allowNull: false,

      notEmpty: {
        msg: "Le prénom ne peut pas être vide.",
      },
      notNull: {
        msg: "Le prénom est une propriété obligatoire",
      },
    },
    lastname: {
      type: DataTypes.STRING,
      allowNull: false,
      validate: {
        notEmpty: {
          msg: "Le nom de famille ne peut pas être vide.",
        },
        notNull: {
          msg: "Le nom de famille est une propriété obligatoire",
        },
      },
    },
  });
};
